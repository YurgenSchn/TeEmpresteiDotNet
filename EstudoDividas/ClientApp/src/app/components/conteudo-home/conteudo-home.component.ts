import { Component, OnInit, Inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';

@Component({
  selector: 'app-conteudo-home',
  templateUrl: './conteudo-home.component.html',
  styleUrls: ['./conteudo-home.component.css']
})



export class ConteudoHomeComponent implements OnInit {

  public payments: Payment[] = [];
  public lended_value: number = 0.0;
  public received_value: number = 0.0;


  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {

    // Setup query parameter
    let userParam = new HttpParams().set('userPublicId', "W26BcRWoklw6E7QU");

    http.get<PaymentsResponse>(baseUrl + 'api/payment/history', { params: userParam }).subscribe(result => {
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



