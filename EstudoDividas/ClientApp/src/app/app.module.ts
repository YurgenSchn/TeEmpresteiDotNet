import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { AppComponent } from './app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NavMenuComponent } from './components/nav-menu/nav-menu.component';
import { CounterComponent } from './components/counter/counter.component';
import { HomeComponent } from './components/home/home.component';
import { LoginComponent } from './components/login/login.component';
import { CreateAccountComponent } from './components/create-account/create-account.component';

import { MainComponent } from './layout/main/main.component';
import { AuthenticationComponent } from './layout/authentication/authentication.component';
import { AuthGuard } from './services/auth.guard';

import { httpInterceptorsProviders } from './http-interceptors';


@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    CounterComponent,
    HomeComponent,
    LoginComponent,
    CreateAccountComponent,
    MainComponent,
    AuthenticationComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot([

       // Layout MAIN - aplicação logada
      {
        path: '',
        component: MainComponent,
        children: [
          { path: '', component: HomeComponent}
        ],
        canActivate: [AuthGuard]
      },

      // Layout Authentication - Deslogado (login e create account)
      {
        path: '',
        component: AuthenticationComponent,
        children: [
          { path: '', redirectTo: "login", pathMatch: 'full'},
          { path: 'login', component: LoginComponent },
          { path: 'create-account', component: CreateAccountComponent },

        ]
      },

    ])
  ],
  providers: [httpInterceptorsProviders],
  bootstrap: [AppComponent]
})
export class AppModule { }
