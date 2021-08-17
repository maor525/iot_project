import { Component } from '@angular/core';
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Student } from '../student';

@Component({
  selector: 'app-modify-form',
  templateUrl: './modify-form.component.html',
  styleUrls: ['../add-form//add-form.component.css']
})

export class ModifyFormComponent {
  private readonly httpOptions = { headers: new HttpHeaders({ "Content-Type": "application/json" }) };
  private readonly modifyUrl = "https://schoolprojfunctions.azurewebsites.net/api/add_student/";
  private readonly getStudentUrl = "https://schoolprojfunctions.azurewebsites.net/api/get_student/";
  constructor(private readonly http: HttpClient) {}
  
  grades = ['7a', '7b', '7c', '8a', '8b', '8c', '9a', '9b', '9c'];

  model = new Student('', '', '', '', '', '', '');

  submitted = false;
  hasStudent = false;
  msg = '';
  info=[''];
  // Get student info
  GetStudent() {
    this.msg='';
    this.http.get(this.getStudentUrl+this.model.id, {responseType: "text"})
    .subscribe(data => {
      if (data == "id not found"){
          this.hasStudent=false;
          this.msg = data;
    } 
    else{
      this.msg = '';
      this.hasStudent = true;
  
      this.info = data.split(" ");
      this.model.name=this.info[1];
      this.model.parentid=this.info[2];
      this.model.classid=this.info[3];
      this.model.password=this.info[4]; 
      this.model.parent_name=this.info[5]; 
      this.model.parent_pw=this.info[6]; 
      }
    });
  }
  
  // Modify student in azure
  ModifyStudent() {
    this.submitted = true;

    const body = this.model.id+"/modify/"+this.model.name+"/"+
    this.model.classid+"/"+this.model.parentid+"/"+this.model.password+"/"+this.model.parent_name+"/"+this.model.parent_pw;

    this.http.get(this.modifyUrl+body, {responseType: 'text'})
      .subscribe(data => this.msg = data);

  }
}

/*
Copyright Google LLC. All Rights Reserved.
Use of this source code is governed by an MIT-style license that
can be found in the LICENSE file at https://angular.io/license
*/
