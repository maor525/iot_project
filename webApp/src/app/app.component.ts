import { stringify } from '@angular/compiler/src/util';
import { Component, Input } from '@angular/core';
import { FormControl, FormGroup, NgForm } from '@angular/forms';
import { ɵangular_packages_platform_browser_platform_browser_d } from '@angular/platform-browser';
import { FormBuilder } from '@angular/forms';
import { formatCurrency } from '@angular/common';
import { from } from 'rxjs';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./add-form/add-form.component.css']
})
export class AppComponent {
  nav = '';
  Permission = false;
  model = '';
  wrong=false;
  CheckPermission(SendRequest: NgForm) {
      if(this.model == 'qwerty'){
          this.Permission = true;
          SendRequest.reset();
          this.wrong=false;
      }
      else{
        this.wrong=true;
      }
      
  }
}

/*
Copyright Google LLC. All Rights Reserved.
Use of this source code is governed by an MIT-style license that
can be found in the LICENSE file at https://angular.io/license
*/
