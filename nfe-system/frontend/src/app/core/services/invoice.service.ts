import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Invoice, CreateInvoiceDto } from '../models/invoice.model';

@Injectable({ providedIn: 'root' })
export class InvoiceService {
  private readonly apiUrl = 'http://localhost:5002/api/invoices';

  constructor(private http: HttpClient) {}

  getAll(): Observable<Invoice[]> {
    return this.http.get<Invoice[]>(this.apiUrl);
  }

  create(dto: CreateInvoiceDto): Observable<Invoice> {
    const headers = new HttpHeaders({
      'Idempotency-Key': crypto.randomUUID(),
    });
    return this.http.post<Invoice>(this.apiUrl, dto, { headers });
  }

  print(id: number): Observable<Invoice> {
    return this.http.post<Invoice>(`${this.apiUrl}/${id}/print`, {});
  }
}
