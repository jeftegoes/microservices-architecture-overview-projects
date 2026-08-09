import { Injectable } from '@angular/core';
import { Amplify, Auth } from 'aws-amplify';
import { BehaviorSubject } from 'rxjs';
import { environment } from '../environments/environment';
import { IUser } from './interfaces/IUser';

@Injectable({
  providedIn: 'root',
})
export class CognitoService {
  public authenticationSubject: BehaviorSubject<any>;

  constructor() {
    Amplify.configure({
      Auth: environment.cognito,
    });

    this.authenticationSubject = new BehaviorSubject<boolean>(false);
  }

  public signUp(user: IUser): Promise<any> {
    return Auth.signUp({
      username: user.email,
      password: user.password,
    });
  }

  public confirmSignUp(user: IUser): Promise<any> {
    return Auth.confirmSignUp(user.email, user.code);
  }

  public signIn(user: IUser): Promise<any> {
    return Auth.signIn(user.email, user.password).then((res) => {
      if (res.challengeName == 'NEW_PASSWORD_REQUIRED')
        Auth.completeNewPassword(res, user.password);

      localStorage.setItem(
        'cognitoIdToken',
        res.signInUserSession.idToken.jwtToken
      );
      this.authenticationSubject.next(true);
    });
  }

  public signOut(): Promise<any> {
    return Auth.signOut().then(() => {
      this.authenticationSubject.next(false);
    });
  }

  public getUser(): Promise<any> {
    return Auth.currentUserInfo();
  }

  public getSession(): Promise<any> {
    return Auth.currentSession();
  }

  public updateUser(user: IUser): Promise<any> {
    return Auth.currentUserPoolUser().then((cognitoUser: any) => {
      return Auth.updateUserAttributes(cognitoUser, user);
    });
  }
}
