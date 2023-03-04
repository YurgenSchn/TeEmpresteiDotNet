import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AccountService } from '../../services/account.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {

  login = {
    username: '',
    password: ''
  };

  constructor(
    private accountService: AccountService,
    private router: Router
  ) { }

  ngOnInit(): void {
  }

  async onSubmit() {
    try {
      // Faz o request pelo service
      const result = await this.accountService.login(this.login);
      console.log(`Login efetuado: ${result}`);

      // Com a tag, navega para o endereço vazio. Vai repassar pelo guard
      this.router.navigate(['']);

    } catch (error) {
      console.error(error)
    }
  }

}
