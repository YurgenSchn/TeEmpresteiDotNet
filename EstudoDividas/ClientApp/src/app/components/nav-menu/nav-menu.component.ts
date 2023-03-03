import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AccountService } from '../../services/account.service';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent {

  public userName: string;

  constructor(private accountService: AccountService, private router: Router) {

    let name = accountService.getUserName();
    this.userName = name.charAt(0).toUpperCase() + name.slice(1);

  }

  isExpanded = false;

  collapse() {
    this.isExpanded = false;
  }

  toggle() {
    this.isExpanded = !this.isExpanded;
  }

  onExit() {
    this.accountService.clearLoginLocalStorage();
  }
}
