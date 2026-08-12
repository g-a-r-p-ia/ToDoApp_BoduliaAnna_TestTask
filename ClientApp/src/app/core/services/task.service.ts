import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class TaskService {
  private readonly apiUrl = 'http://localhost:5000/api/tasks';
  private readonly categoriesUrl = 'http://localhost:5000/api/categories';

  constructor(private http: HttpClient) {}

  getTasks(
    pageNumber: number = 1,
    pageSize: number = 10,
    categoryId?: string,
    searchTerm?: string,
    isCompleted?: boolean | null
  ): Observable<any> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (categoryId) {
      params = params.set('categoryId', categoryId);
    }

    if (searchTerm && searchTerm.trim()) {
      params = params.set('searchTerm', searchTerm.trim());
    }

    if (isCompleted !== null && isCompleted !== undefined) {
      params = params.set('isCompleted', isCompleted);
    }

    return this.http.get<any>(this.apiUrl, { params });
  }

  getCategories(): Observable<any[]> {
    return this.http.get<any[]>(this.categoriesUrl);
  }

  addTask(task: { title: string; categoryId: string }): Observable<any> {
    return this.http.post<any>(this.apiUrl, task);
  }

  updateTask(id: string, task: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${id}`, task);
  }

  deleteTask(id: string): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/${id}`);
  }
}
