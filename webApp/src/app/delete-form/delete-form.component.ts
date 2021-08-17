import { Component } from '@angular/core';
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Student } from '../student';

@Component({
  selector: 'app-delete-form',
  templateUrl: './delete-form.component.html',
  styleUrls: ['../add-form//add-form.component.css']
})
export class DeleteFormComponent {
  private readonly httpOptions = { headers: new HttpHeaders({ "Content-Type": "application/json" }) };
  private readonly modifyUrl = "https://schoolprojfunctions.azurewebsites.net/api/add_student/";
  constructor(private readonly http: HttpClient) {}

  model='';

  submitted = false;

  msg='';

  DeleteStudent() {
    this.submitted = true;

    const body = this.model+"/remove/1/1/1/1/1/1";

    this.http.get(this.modifyUrl+body, {responseType: 'text'})
      .subscribe(data => this.msg = data);

  }
}

/*
Copyright Google LLC. All Rights Reserved.
Use of this source code is governed by an MIT-style license that
can be found in the LICENSE file at https://angular.io/license
*/
