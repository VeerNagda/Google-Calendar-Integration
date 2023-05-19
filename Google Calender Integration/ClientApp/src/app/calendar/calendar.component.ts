import {Component, OnInit} from '@angular/core';
import {FormBuilder, FormGroup, Validators} from '@angular/forms';
import {HttpClient} from "@angular/common/http";
import {DatePipe} from "@angular/common";

@Component({
  selector: 'app-calendar',
  templateUrl: './calendar.component.html',
  styleUrls: ['./calendar.component.css']
})
export class CalendarComponent implements OnInit {

  eventForm!: FormGroup;
  private _messageModule: MessageModule = new MessageModule();

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
    this.http.get<MessageModule>("https://localhost:44416/api/calendarevent/hasaccesstoken").subscribe(result =>{
      this._messageModule = result;
      if (this._messageModule.status) {
        if (!eventData.recurring) {
          return this.http.post<MessageModule>('https://localhost:44416/api/calendarevent/eventcreate', this.eventForm.value).subscribe(result =>{
            this._messageModule = result;
            if(this._messageModule.status == "Success")
              alert(this._messageModule.message)
            else
              alert(this._messageModule.message)
          });
        } else {
          return this.http.post<MessageModule>('https://localhost:44416/api/calendarevent/recurringevent', this.eventForm.value).subscribe(result =>{
            this._messageModule = result;
            if(this._messageModule.status == "Success")
              alert(this._messageModule.message)
            else
              alert(this._messageModule.message)
          });
        }
      } else {
        alert("Please Login to proceed. Visit Url https://localhost:44416/api/login")
        window.location.replace("https://localhost:44416/api/login");
        return ;
      }
    })
  }
}

class MessageModule{
  get message(): string {
    return this._message;
  }

  set message(value: string) {
    this._message = value;
  }
  get status(): string {
    return this._status;
  }

  set status(value: string) {
    this._status = value;
  }
  protected _status: string='';
  protected _message: string='';
}
