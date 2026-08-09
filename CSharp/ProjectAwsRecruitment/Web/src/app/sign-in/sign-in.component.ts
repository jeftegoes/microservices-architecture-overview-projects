import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { CognitoService } from '../cognito.service';
import { IUser } from '../interfaces/IUser';

@Component({
  selector: 'app-sign-in',
  templateUrl: './sign-in.component.html',
  styleUrls: ['./sign-in.component.scss'],
})
export class SignInComponent {
  user: IUser;
  loading: boolean;

  constructor(private router: Router, private cognitoService: CognitoService) {
    this.loading = false;
    this.user = {} as IUser;
  }

  signIn() {
    this.loading = true;
    this.cognitoService
      .signIn(this.user)
      .then(() => {
        this.router.navigate(['/profile']);
      })
      .catch(() => {
        this.loading = false;
      });
  }
}
