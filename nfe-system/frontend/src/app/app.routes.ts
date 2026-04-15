import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'products', pathMatch: 'full' },
  {
    path: 'products',
    loadComponent: () => import('./pages/products/products').then((m) => m.ProductsComponent),
  },
  {
    path: 'invoices',
    loadComponent: () => import('./pages/invoices/invoices').then((m) => m.InvoicesComponent),
  },
];
