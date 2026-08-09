import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class RecruiterService {
  private apiUrl =
    'https://0w26cjnetj.execute-api.sa-east-1.amazonaws.com/test/recruiter';

  constructor(private http: HttpClient) {}

  saveCandidate(data: { [key: string]: any }, file: File) {
    const formData: FormData = new FormData();
    formData.append('file', file, file.name);

    Object.keys(data).forEach((key) => {
      formData.append(key, data[key]);
    });

    const headers = new HttpHeaders();

    return this.http.post(this.apiUrl, formData, { headers });
  }
}
