import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

export interface ApiResponse<T = any> {
  success: boolean;
  message: string;
  data: T;
  errors?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  /**
   * GET request
   */
  get<T>(endpoint: string, options?: any): Observable<ApiResponse<T>> {
    return this.http.get<ApiResponse<T>>(`${this.apiUrl}/${endpoint}`, { ...options }).pipe(
      map((response: any) => response),
      catchError(this.handleError)
    );
  }

  /**
   * GET request for blob (file downloads)
   */
  getBlob(endpoint: string, options?: any): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${endpoint}`, { 
      ...options,
      responseType: 'blob'
    }).pipe(
      catchError(this.handleError)
    ) as unknown as Observable<Blob>;
  }

  /**
   * POST request
   */
  post<T>(endpoint: string, body: any, options?: any): Observable<ApiResponse<T>> {
    return this.http.post<ApiResponse<T>>(`${this.apiUrl}/${endpoint}`, body, { ...options }).pipe(
      map((response: any) => response),
      catchError(this.handleError)
    );
  }

  /**
   * PUT request
   */
  put<T>(endpoint: string, body: any, options?: any): Observable<ApiResponse<T>> {
    return this.http.put<ApiResponse<T>>(`${this.apiUrl}/${endpoint}`, body, { ...options }).pipe(
      map((response: any) => response),
      catchError(this.handleError)
    );
  }

  /**
   * DELETE request
   */
  delete<T>(endpoint: string, options?: any): Observable<ApiResponse<T>> {
    return this.http.delete<ApiResponse<T>>(`${this.apiUrl}/${endpoint}`, { ...options }).pipe(
      map((response: any) => response),
      catchError(this.handleError)
    );
  }

  /**
   * PATCH request
   */
  patch<T>(endpoint: string, body: any, options?: any): Observable<ApiResponse<T>> {
    return this.http.patch<ApiResponse<T>>(`${this.apiUrl}/${endpoint}`, body, { ...options }).pipe(
      map((response: any) => response),
      catchError(this.handleError)
    );
  }

  /**
   * Get API base URL (useful for dynamic endpoints)
   */
  getBaseUrl(): string {
    return this.apiUrl;
  }

  /**
   * Handle errors
   */
  private handleError(error: any) {
    let errorMessage = 'An error occurred';

    if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = error.error.message;
    } else if (error.status) {
      // Server-side error
      errorMessage = error.error?.message || `Server error: ${error.status}`;
    }

    console.error('API Error:', errorMessage, error);
    return throwError(() => ({
      status: error.status,
      message: errorMessage,
      details: error.error
    }));
  }
}
