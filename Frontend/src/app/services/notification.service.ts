import { Injectable } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
  
@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  
  constructor(private toastr: ToastrService) { }
  
  showSuccess(message : any, title: any){
      this.toastr.success(message, title, { timeOut: 10000 })
  }
  
  showError(message : any, title: any){
      this.toastr.error(message, title, { disableTimeOut: true, closeButton: true})
  }
  
  showInfo(message : any, title: any){
      this.toastr.info(message, title, { timeOut: 10000 })
  }
  
  showWarning(message : any, title: any){
      this.toastr.warning(message, title, { timeOut: 10000 })
  }
}
