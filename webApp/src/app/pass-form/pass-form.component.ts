import { Component } from '@angular/core';
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Student } from '../student';

@Component({
  selector: 'app-pass-form',
  templateUrl: './pass-form.component.html',
  styleUrls: ['../add-form//add-form.component.css']
})
export class PassFormComponent {
  private readonly httpOptions = { headers: new HttpHeaders({ "Content-Type": "application/json" }) };
  private readonly passUrl = "https://schoolprojfunctions.azurewebsites.net/api/gate_permission/";
  constructor(private readonly http: HttpClient) {}

  model = '';
  submitted=false
  msg='';
  
  Allow() {
    this.submitted=true;
    this.http.get(this.passUrl+this.model, {responseType: 'text'})
    .subscribe(data => this.msg = data);
  }
}

/*
Copyright Google LLC. All Rights Reserved.
Use of this source code is governed by an MIT-style license that
can be found in the LICENSE file at https://angular.io/license
*/
