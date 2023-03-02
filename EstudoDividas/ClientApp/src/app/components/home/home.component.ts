import { Component, OnInit, Inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { AccountService } from '../../services/account.service';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-conteudo-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})



export class HomeComponent implements OnInit {

  public payments: Payment[] = [];
  public lended_value: number = 0.0;
  public received_value: number = 0.0;


  constructor(http: HttpClient, accountService: AccountService) {

    let ids = accountService.getUserIds()

    // Setup query parameter
    let params = new HttpParams();
    params = params.append('userPublicId',  ids.userPublicId);
    params = params.append('userPrivateId', ids.userPrivateId);

    http.get<PaymentsResponse>(`${environment.api}/api/payment/history`, { params: params }).subscribe(result => {
      this.payments = result.payments;
      this.calculate_values();
    }, error => console.error(error));


  }

  ngOnInit(): void {
    
  }

  calculate_values() {

    let send_value: number = 0;
    let receive_value: number = 0;

    this.payments.forEach(p => {
      // Check if user received or pay
      if (p.sender_id == "W26BcRWoklw6E7QU") {
        send_value += p.value;
      } else {
        receive_value += p.value;
      }
    })

    this.lended_value = send_value;
    this.received_value = receive_value;

  }

}

interface PaymentsResponse {
  payments: Payment[],
  message: string,
  status: string
}

interface Payment {
  id            : number;
  sender_id     : string;
  sender_name   : string;
  receiver_id   : string;
  receiver_name : string;
  value         : number;
  description   : string;
  date          : string;
  confirmed     : number;
}



