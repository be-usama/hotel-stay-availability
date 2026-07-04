import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { SearchResponse, ReservationRequest, ReservationConfirmation } from '../models/hotel';

@Injectable({
  providedIn: 'root'
})
export class HotelService {
  private apiUrl = 'http://localhost:5000/hotels';

  constructor(private http: HttpClient) { }

  searchHotels(destination: string, checkIn: string, checkOut: string, roomType?: string): Observable<SearchResponse> {
    let url = `${this.apiUrl}/search?destination=${destination}&checkIn=${checkIn}&checkOut=${checkOut}`;
    if (roomType) {
      url += `&roomType=${roomType}`;
    }
    return this.http.get<SearchResponse>(url);
  }

  reserveRoom(request: ReservationRequest): Observable<ReservationConfirmation> {
    return this.http.post<ReservationConfirmation>(`${this.apiUrl}/reserve`, request);
  }

  getReservation(referenceNumber: string): Observable<ReservationConfirmation> {
    return this.http.get<ReservationConfirmation>(`${this.apiUrl}/reservation/${referenceNumber}`);
  }
}
