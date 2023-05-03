import {Component, OnInit} from '@angular/core';
import {FormBuilder, FormGroup, Validators} from '@angular/forms';
import {HttpClient} from "@angular/common/http";
import {DatePipe} from "@angular/common";
import {catchError, switchMap, throwError} from "rxjs";

@Component({
  selector: 'app-calendar',
  templateUrl: './calendar.component.html',
  styleUrls: ['./calendar.component.css']
})
export class CalendarComponent implements OnInit {

  eventForm!: FormGroup;

  constructor(private http: HttpClient, private fb: FormBuilder, private datePipe: DatePipe) {
  }

  ngOnInit(): void {
    this.eventForm = this.fb.group({
      title: ['', Validators.required],
      description: [''],
      start: ['', Validators.required],
      end: ['', Validators.required],
      location: [''],
      timezone: Intl.DateTimeFormat().resolvedOptions().timeZone,
      recurring: [false],
      recurringOption: [''],
      until: ['']
    });
  }


  onSubmit(): void {

    const eventData = {...this.eventForm.value};
    const startDate = eventData.start;
    const endDate = eventData.end;
    const untilDate = eventData.until;
    //converts the date & time to asp.net datetime acceptable format
    eventData.start = this.datePipe.transform(startDate, 'dd-MM-yyyy HH:mm:ss')!;
    eventData.end = this.datePipe.transform(endDate, 'dd-MM-yyyy HH:mm:ss')!;
    eventData.until = this.datePipe.transform(untilDate, 'dd-MM-yyyy HH:mm:ss')!;
    //checks if the token is available or not
    this.http.get("https://localhost:44416/api/calendarevent/hasaccesstoken").pipe(
      switchMap(result => {
        if (result) {
          if (!eventData.recurring) {
            return this.http.post('https://localhost:44416/api/calendarevent/eventcreate', this.eventForm.value);
          } else {
            return this.http.post('https://localhost:44416/api/calendarevent/recurringevent', this.eventForm.value);
          }
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
