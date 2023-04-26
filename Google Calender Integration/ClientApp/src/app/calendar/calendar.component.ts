import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import {HttpClient} from "@angular/common/http";
import {DatePipe} from "@angular/common";

@Component({
  selector: 'app-calendar',
  templateUrl: './calendar.component.html',
  styleUrls: ['./calendar.component.css']
})
export class CalendarComponent implements OnInit {

  eventForm!: FormGroup;
  constructor(private http: HttpClient, private fb: FormBuilder,private datePipe: DatePipe) {
  }

  ngOnInit(): void {
    this.eventForm = this.fb.group({
      title: ['', Validators.required],
      description: [''],
      start: ['', Validators.required],
      end: ['', Validators.required],
      location: [''],
      timezone: Intl.DateTimeFormat().resolvedOptions().timeZone,
    });
  }

  onSubmit(): void {
    const eventData = {...this.eventForm.value};
    const startDate = eventData.start;
    const endDate = eventData.end;
    eventData.start = this.datePipe.transform(startDate, 'dd-MM-yyyy HH:mm:ss')!;
    eventData.end = this.datePipe.transform(endDate, 'dd-MM-yyyy HH:mm:ss')!;
    console.log(this.eventForm.value)
    this.http.get("https://localhost:44416/api/calendarevent/hasaccesstoken").subscribe(result => {
      if (result){
        this.http.post('https://localhost:44416/api/calendarevent/eventcreate', this.eventForm.value).subscribe(result => {
          console.log(result);
        });
      } else {
        this.http.get('https://localhost:44416/api/login', this.eventForm.value).subscribe(result => {
          console.log(result);
        });
      }
    })


  }

}
