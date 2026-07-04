import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HotelService } from '../services/hotel.service';
import { ReservationRequest, ReservationConfirmation, Room, DocumentType } from '../models/hotel';

@Component({
  selector: 'app-reservation',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './reservation.component.html',
  styleUrls: ['./reservation.component.css']
})
export class ReservationComponent {
  room: Room | null = null;
  guestName = '';
  documentType: DocumentType = DocumentType.Passport;
  documentNumber = '';
  confirmation: ReservationConfirmation | null = null;
  error: string | null = null;
  loading = false;

  DocumentType = DocumentType;

  constructor(private hotelService: HotelService) { }

  setRoom(room: Room) {
    this.room = room;
    this.error = null;
    this.confirmation = null;
  }

  reserve() {
    this.error = null;
    if (!this.room || !this.guestName || !this.documentNumber) {
      this.error = 'All fields are required';
      return;
    }

    const request: ReservationRequest = {
      roomId: this.room.roomId,
      guestName: this.guestName,
      documentType: this.documentType,
      documentNumber: this.documentNumber,
      destination: '', // Will be set from context
      checkIn: '', // Will be set from context
      checkOut: '' // Will be set from context
    };

    this.loading = true;
    this.hotelService.reserveRoom(request)
      .subscribe({
        next: (data) => {
          this.confirmation = data;
          this.room = null;
          this.guestName = '';
          this.documentNumber = '';
          this.loading = false;
        },
        error: (err) => {
          this.error = err.error?.error || 'Reservation failed';
          this.loading = false;
        }
      });
  }

  cancel() {
    this.room = null;
    this.guestName = '';
    this.documentNumber = '';
    this.error = null;
  }
}
