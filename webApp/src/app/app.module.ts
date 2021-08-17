import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http'
import { AppComponent } from './app.component';
import { AddFormComponent } from './add-form/add-form.component';
import { DeleteFormComponent } from './delete-form/delete-form.component';
import { ModifyFormComponent } from './modify-form/modify-form.component';
import { PassFormComponent } from './pass-form/pass-form.component';
@NgModule({
  imports: [BrowserModule, CommonModule, HttpClientModule, FormsModule],
  declarations: [
    AppComponent,
    ModifyFormComponent,
    AddFormComponent,
    DeleteFormComponent,
    PassFormComponent
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule {}

/*
Copyright Google LLC. All Rights Reserved.
Use of this source code is governed by an MIT-style license that
can be found in the LICENSE file at https://angular.io/license
*/
