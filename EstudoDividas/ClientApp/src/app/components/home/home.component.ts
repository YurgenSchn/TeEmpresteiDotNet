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
  public lended: number = 0.0;
  public lended_unconfirmed: number = 0.0;
  public received: number = 0.0;
  public received_unconfirmed: number = 0.0;
  public userInfo;
  private http;


  constructor(http: HttpClient, accountService: AccountService) {

    this.http = http;
    this.userInfo = accountService.getUserIds();

    this.getCompleteHistory();
  }

  ngOnInit(): void {
    
  }

  getCompleteHistory() {
    // Setup query parameter
    let params = new HttpParams();
    params = params.append('userPublicId', this.userInfo.userPublicId);
    params = params.append('userPrivateId', this.userInfo.userPrivateId);

    this.http.get<PaymentsResponse>(`${environment.api}/api/payment/history`, { params: params }).subscribe(result => {
      this.payments = result.payments;
      this.calculate_values();
    }, error => console.error(error));
  }

  calculate_values() {

    let lended: number = 0;
    let lended_unconfirmed: number = 0;
    let received: number = 0;
    let received_unconfirmed: number = 0;

    this.payments.forEach(p => {
      // Check if user received or pay
      if (p.sender_id == this.userInfo.userPublicId)
        if(p.confirmed)
          lended += p.value;
        else
          lended_unconfirmed += p.value;
      else
        if (p.confirmed)
          received += p.value;
        else
          received_unconfirmed += p.value;

    })

    this.lended = lended;
    this.lended_unconfirmed = lended_unconfirmed;
    this.received = received;
    this.received_unconfirmed = received_unconfirmed;

  }

  confirmPayment(payment_id: number) {
    // Setup request object
    let requestObj : any = {
      userPrivateId: this.userInfo.userPrivateId,
      userPublicId:  this.userInfo.userPublicId,
      paymentId: payment_id
    };

    this.http.post<PaymentsResponse>(`${environment.api}/api/payment/confirm`, requestObj).subscribe(
      result => {
        window.alert("Pagamento Confirmado!");
        this.getCompleteHistory();
      }, error => {
        window.alert("Ocorreu um erro.")
        console.error(error);
      }
    );
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



