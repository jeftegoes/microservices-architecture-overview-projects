import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { CognitoService } from '../cognito.service';
import { IUser } from '../interfaces/IUser';

@Component({
  selector: 'app-sign-up',
  templateUrl: './sign-up.component.html',
  styleUrls: ['./sign-up.component.scss'],
})
export class SignUpComponent {
  user: IUser;
  isConfirm: boolean;
  loading: boolean;

  constructor(private router: Router, private cognitoService: CognitoService) {
    this.loading = false;
    this.isConfirm = false;
    this.user = {} as IUser;
  }

  signUp() {
    this.loading = true;
    this.cognitoService
      .signUp(this.user)
      .then(() => {
        this.loading = false;
        this.isConfirm = true;
      })
      .catch(() => {
        this.loading = false;
        this.isConfirm = false;
      });
  }

  public confirmSignUp(): void {
    this.loading = true;
    this.cognitoService
      .confirmSignUp(this.user)
      .then(() => {
        this.router.navigate(['/signIn']);
      })
      .catch(() => {
        this.loading = false;
      });
  }
}
