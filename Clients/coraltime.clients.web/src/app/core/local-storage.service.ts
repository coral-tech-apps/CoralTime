import { Injectable } from "@angular/core";

@Injectable({ providedIn: 'root' })
export class LocalStorageService {
  
  setItem<T>(key: string, value: T): void {
    localStorage.setItem(key, JSON.stringify(value));
  }

  getItem<T>(key: string): T | null {
    const item = localStorage.getItem(key);
  
    if (!item) {
      return null;
    }

    return JSON.parse(item) as T;
  }

  removeItem(key: string): void {
    localStorage.removeItem(key);
  }
}