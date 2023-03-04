import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AccountService } from '../../services/account.service';

@Component({
  selector: 'app-create-account',
  templateUrl: './create-account.component.html',
  styleUrls: ['./create-account.component.css']
})
export class CreateAccountComponent implements OnInit {

  account = {
    name: "",
    email: "",
    username: "",
    password: ""
  };

  constructor(private router: Router, private accountService: AccountService) { }

  ngOnInit(): void {
  }

  async onSubmit() {
    try {
      // Faz o request pelo service
      const registerResult = await this.accountService.register(this.account);

      if (registerResult) {
        const loginResult = await this.accountService.login({ username: this.account.username, password: this.account.password });
        console.log(`Login efetuado: ${loginResult}`);
      }
      

      // Com a tag, navega para o endereço vazio. Vai repassar pelo guard
      this.router.navigate(['']);

    } catch (error) {
      console.error(error)
    }

  }

}
