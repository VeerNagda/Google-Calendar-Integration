import { Component } from '@angular/core';
import {HttpClient} from "@angular/common/http";
import {FormBuilder, Validators} from "@angular/forms";
import {DatePipe} from "@angular/common";
import {catchError, switchMap, throwError} from "rxjs";

@Component({
  selector: 'app-task',
  templateUrl: './task.component.html',
  styleUrls: ['./task.component.css']
})
export class TaskComponent {
  eventForm: any;
  constructor(private http: HttpClient, private fb: FormBuilder, private datePipe: DatePipe) {
  }

  ngOnInit(): void{
    this.eventForm = this.fb.group({
      title: ['', Validators.required],
      notes: [''],
      due: ['', Validators.required],
      recurring: false,
      recurrence: [''],
      until:[''],
    })
  }

  onSubmit() {
    const eventData = {...this.eventForm.value};
    eventData.due = this.datePipe.transform(eventData.due, 'dd-MM-yyyy HH:mm:ss')!;
    this.http.get("https://localhost:44416/api/task/hasaccesstoken").pipe(
      switchMap(result => {
        if (result) {
          console.log(this.eventForm.value)
            return this.http.post('https://localhost:44416/api/task/createtask', this.eventForm.value);
        } else {
          alert("Please Login to proceed. Visit Url https://localhost:44416/api/login")
          return this.http.get('https://localhost:44416/api/login');
        }
      }),
      catchError(error => {
        console.error(error);
        return throwError(error);
      })
    ).subscribe(result => {
      console.log(result);
    });
  }
}
