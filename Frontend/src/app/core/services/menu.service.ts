import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response';

export interface MenuRawItem {
  id: number;
  nombre: string;
  path: string;
  descripcion: string;
  padreId: number;
  visible: boolean;
}

export interface NavItem extends MenuRawItem {
  children?: NavItem[];
}

@Injectable({ providedIn: 'root' })
export class MenuService {
  private readonly http = inject(HttpClient);
  readonly navItems = signal<NavItem[]>([]);

  fetchMenu(usuario: string) {
    const url = `${environment.apiBase}/Menu/items?usuario=${usuario}&sistema=${environment.noSistema}`;
    this.http.get<ApiResponse<MenuRawItem[]>>(url).subscribe(res => {
      if (res.success && res.data) {
        this.navItems.set(this.buildHierarchy(res.data));
      }
    });
  }

  private buildHierarchy(items: MenuRawItem[]): NavItem[] {
    const map = new Map<number, NavItem>();
    const roots: NavItem[] = [];
    items.forEach(item => map.set(item.id, { ...item, children: [] }));
    items.forEach(item => {
      if (item.padreId === 0) {
        roots.push(map.get(item.id)!);
      } else {
        const parent = map.get(item.padreId);
        if (parent) parent.children?.push(map.get(item.id)!);
      }
    });
    return roots;
  }
}
