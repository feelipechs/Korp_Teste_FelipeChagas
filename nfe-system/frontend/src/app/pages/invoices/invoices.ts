import { Component, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { MatSelectModule } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import { CommonModule } from '@angular/common';
import { InvoiceService } from '../../core/services/invoice.service';
import { ProductService } from '../../core/services/product.service';
import { Invoice, CreateInvoiceItemDto } from '../../core/models/invoice.model';
import { Product } from '../../core/models/product.model';

@Component({
  selector: 'app-invoices',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatTableModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatCardModule,
    MatSelectModule,
    MatChipsModule,
  ],
  templateUrl: './invoices.html',
  styleUrl: './invoices.scss',
})
export class InvoicesComponent implements OnInit {
  invoices = signal<Invoice[]>([]);
  products = signal<Product[]>([]);
  pendingItems = signal<CreateInvoiceItemDto[]>([]);
  printing = signal<number | null>(null);
  loading = signal(false);

  itemForm: FormGroup;
  displayedColumns = ['number', 'status', 'createdAt', 'items', 'actions'];

  constructor(
    private invoiceService: InvoiceService,
    private productService: ProductService,
    private fb: FormBuilder,
    private snackBar: MatSnackBar,
  ) {
    this.itemForm = this.fb.group({
      product: [null, Validators.required],
      quantity: [1, [Validators.required, Validators.min(1)]],
    });
  }

  ngOnInit(): void {
    this.loadInvoices();
    this.loadProducts();
  }

  loadInvoices(): void {
    this.invoiceService.getAll().subscribe({
      next: (data) => this.invoices.set(data),
      error: () =>
        this.snackBar.open('Erro ao carregar notas fiscais.', 'Fechar', { duration: 3000 }),
    });
  }

  loadProducts(): void {
    this.productService.getAll().subscribe({
      next: (data) => this.products.set(data),
    });
  }

  addItem(): void {
    if (this.itemForm.invalid) return;
    const { product, quantity } = this.itemForm.value;
    const already = this.pendingItems().find((i) => i.productId === product.id);
    if (already) {
      this.snackBar.open('Produto já adicionado.', 'Fechar', { duration: 2000 });
      return;
    }
    this.pendingItems.update((items) => [
      ...items,
      {
        productId: product.id,
        productCode: product.code,
        productDescription: product.description,
        quantity,
      },
    ]);
    this.itemForm.reset({ quantity: 1 });
  }

  removeItem(productId: number): void {
    this.pendingItems.update((items) => items.filter((i) => i.productId !== productId));
  }

  createInvoice(): void {
    if (this.pendingItems().length === 0) {
      this.snackBar.open('Adicione ao menos um produto.', 'Fechar', { duration: 2000 });
      return;
    }
    this.loading.set(true);
    this.invoiceService.create({ items: this.pendingItems() }).subscribe({
      next: () => {
        this.snackBar.open('Nota fiscal criada!', 'Fechar', { duration: 3000 });
        this.pendingItems.set([]);
        this.loadInvoices();
        this.loading.set(false);
      },
      error: () => {
        this.snackBar.open('Erro ao criar nota fiscal.', 'Fechar', { duration: 3000 });
        this.loading.set(false);
      },
    });
  }

  print(invoice: Invoice): void {
    if (invoice.status !== 'Open') return;
    this.printing.set(invoice.id);
    this.invoiceService.print(invoice.id).subscribe({
      next: () => {
        this.snackBar.open('Nota impressa e fechada com sucesso!', 'Fechar', { duration: 3000 });
        this.printing.set(null);
        this.loadInvoices();
      },
      error: (err) => {
        const msg = err.error?.message ?? 'Erro ao imprimir nota.';
        this.snackBar.open(msg, 'Fechar', { duration: 4000 });
        this.printing.set(null);
      },
    });
  }

  formatItems(invoice: Invoice): string {
    return invoice.items.map((i) => `${i.productCode} (x${i.quantity})`).join(', ');
  }
}
