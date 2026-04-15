export interface InvoiceItem {
  id: number;
  productId: number;
  productCode: string;
  productDescription: string;
  quantity: number;
}

export interface Invoice {
  id: number;
  number: number;
  status: 'Open' | 'Closed';
  createdAt: string;
  items: InvoiceItem[];
}

export interface CreateInvoiceItemDto {
  productId: number;
  productCode: string;
  productDescription: string;
  quantity: number;
}

export interface CreateInvoiceDto {
  items: CreateInvoiceItemDto[];
}