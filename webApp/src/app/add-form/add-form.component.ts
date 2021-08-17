import { Component } from '@angular/core';
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Student } from '../student';
import { NgForm } from '@angular/forms';

@Component({
  selector: 'app-add-form',
  templateUrl: './add-form.component.html',
  styleUrls: ['./add-form.component.css']
})

export class AddFormComponent {
  private readonly httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json'}) };

  private readonly modifyUrl = "https://schoolprojfunctions.azurewebsites.net/api/add_student/";
  constructor(private readonly http: HttpClient) {}
  grades = ['7a', '7b', '7c', '8a', '8b', '8c', '9a', '9b', '9c'];

  model = new Student('', '', '', '', '','', '');
  msg='';
  submitted = false;

  AddStudent() {
    this.submitted = true;

    const body = this.model.id+"/add/"+this.model.name+"/"
    +this.model.classid+"/"+this.model.parentid+"/"+this.model.password+"/"+this.model.parent_name+"/"+this.model.parent_pw;

    this.http.get(this.modifyUrl+body, {responseType: 'text'})
      .subscribe(data => this.msg = data);
  }

  NewStudent(sgateForm: NgForm){
      this.submitted=false;
      this.msg='';
      sgateForm.reset();
  }
}

/*
Copyright Google LLC. All Rights Reserved.
Use of this source code is governed by an MIT-style license that
can be found in the LICENSE file at https://angular.io/license
*/
