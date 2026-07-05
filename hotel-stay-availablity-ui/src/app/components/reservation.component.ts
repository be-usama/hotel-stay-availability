import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HotelService } from '../services/hotel.service';
import { ReservationRequest, ReservationConfirmation, ReservationSelection } from '../models/hotel';
import { DocumentType } from '../models/enums';

@Component({
  selector: 'app-reservation',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './reservation.component.html',
  styleUrl: './reservation.component.scss'
})
export class ReservationComponent {
  @Input() selection: ReservationSelection | null = null;
  @Output() cleared = new EventEmitter<void>();

  guestName = '';
  documentType: DocumentType = DocumentType.Passport;
  documentNumber = '';
  confirmation: ReservationConfirmation | null = null;
  error: string | null = null;
  loading = false;

  DocumentType = DocumentType;

  constructor(private hotelService: HotelService) { }

  get room() {
    return this.selection?.room ?? null;
  }

  reserve() {
    this.error = null;
    const guestName = this.guestName.trim();
    const documentNumber = this.documentNumber.trim();

    if (!this.selection || !guestName || !documentNumber) {
      this.error = 'All fields are required';
      return;
    }

    const request: ReservationRequest = {
      roomId: this.selection.room.roomId,
      guestName,
      documentType: this.documentType,
      documentNumber,
      destination: this.selection.destination,
      checkIn: this.selection.checkIn,
      checkOut: this.selection.checkOut
    };

    this.loading = true;
    this.hotelService.reserveRoom(request)
      .subscribe({
        next: (data) => {
          this.confirmation = data;
          this.guestName = '';
          this.documentNumber = '';
          this.cleared.emit();
          this.loading = false;
        },
        error: (err) => {
          this.error = err.error?.error || 'Reservation failed';
          this.loading = false;
        }
      });
  }

  cancel() {
    this.guestName = '';
    this.documentNumber = '';
    this.error = null;
    this.cleared.emit();
  }

  startNewSearch() {
    this.confirmation = null;
    this.error = null;
    this.cleared.emit();
  }

  formatRoomType(value: ReservationConfirmation['roomType'] | string | number | null | undefined): string {
    if (value === 0 || value === '0') {
      return 'Standard';
    }

    if (value === 1 || value === '1') {
      return 'Deluxe';
    }

    if (value === 2 || value === '2') {
      return 'Suite';
    }

    return value == null || value === '' ? '—' : String(value);
  }

  formatCancellationPolicy(
    value: ReservationConfirmation['cancellationPolicy'] | string | number | null | undefined
  ): string {
    if (value === 0 || value === '0') {
      return 'FreeCancellation';
    }

    if (value === 1 || value === '1') {
      return 'Flexible';
    }

    if (value === 2 || value === '2') {
      return 'NonRefundable';
    }

    return value == null || value === '' ? '—' : String(value);
  }
}
