import { Component } from '@angular/core';
import { CognitoService } from '../cognito.service';
import { ICandidate } from '../interfaces/ICandidate';
import { RecruiterService } from '../recruiter.service';

@Component({
  selector: 'app-admin',
  templateUrl: './admin.component.html',
  styleUrls: ['./admin.component.scss'],
})
export class AdminComponent {
  selectedFile: File | null = null;
candidate: ICandidate;

  constructor(
    private recruiterService: RecruiterService,
    private cognitoService: CognitoService
  ) {
    this.candidate = {} as ICandidate;
  }

  onFileChange(event: any) {
    this.selectedFile = event.target.files[0];
  }

  uploadFile() {
    this.cognitoService.getSession().then((session: any) => {
      this.cognitoService.getUser().then((user: any) => {
        this.candidate.userId = user.attributes.sub;
        this.candidate.idToken = session.accessToken.jwtToken;

        if (this.selectedFile) {
          this.recruiterService.saveCandidate(this.candidate, this.selectedFile).subscribe(
            (response) => {
              console.log('File uploaded successfully!', response);
            },
            (error) => {
              console.error('Error uploading file:', error);
            }
          );
        }
      });
    });
  }
}
