import { Injectable } from "@angular/core";
import { LocalStorageService } from "./local-storage.service";
import { BROWSER_STORAGE_KEYS } from "./constant.service";

@Injectable({ providedIn: 'root' })
export class StatePersistenceService {
  private existedKeys = BROWSER_STORAGE_KEYS.QUERIES;
  
  constructor(
    private localStorage: LocalStorageService) {}

  setState<T>(key: keyof typeof BROWSER_STORAGE_KEYS.QUERIES, value: T): void {
    this.localStorage.setItem(this.existedKeys[key], value);
  }

  getState<T>(key: keyof typeof BROWSER_STORAGE_KEYS.QUERIES): T | null {
    return this.localStorage.getItem(this.existedKeys[key]);
  }

  removeState(key: keyof typeof BROWSER_STORAGE_KEYS.QUERIES): void {
    this.localStorage.removeItem(this.existedKeys[key]);
  }
}
