import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';

type PostParams = Parameters<HttpClient['post']>;
type GetParams = Parameters<HttpClient['get']>;
type PutParams = Parameters<HttpClient['put']>;
type DeleteParams = Parameters<HttpClient['delete']>;
type PatchParams = Parameters<HttpClient['patch']>;

@Injectable({
  providedIn: 'root'
})
export class HttpService {
  private readonly httpClient = inject(HttpClient);

  public get<T>(...params: GetParams) {
    return this.httpClient.get<T>(...params);
  }

  public post<T>(...params: PostParams) {
    return this.httpClient.post<T>(...params);
  }

  public put<T>(...params: PutParams) {
    return this.httpClient.put<T>(...params);
  }

  public delete<T>(...params: DeleteParams) {
    return this.httpClient.delete<T>(...params);
  }

  public patch<T>(...params: PatchParams) {
    return this.httpClient.patch<T>(...params);
  }
}
