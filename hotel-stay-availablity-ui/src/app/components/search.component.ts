import { Component, EventEmitter, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HotelService } from '../services/hotel.service';
import { SearchResponse, Room, ReservationSelection } from '../models/hotel';

@Component({
  selector: 'app-search',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './search.component.html',
  styleUrl: './search.component.scss'
})
export class SearchComponent {
  @Output() roomSelected = new EventEmitter<ReservationSelection>();

  sortField: 'totalStayPrice' | 'starRating' = 'totalStayPrice';
  sortDirection: 'asc' | 'desc' = 'asc';
  destination = '';
  checkIn = '';
  checkOut = '';
  roomType = '';
  results: SearchResponse | null = null;
  error: string | null = null;
  loading = false;

  constructor(private hotelService: HotelService) { }

  get today(): string {
    return new Date().toISOString().split('T')[0];
  }

  selectRoom(room: Room) {
    if (!this.results) {
      return;
    }

    this.roomSelected.emit({
      room,
      destination: this.results.destination,
      checkIn: this.results.checkIn.split('T')[0],
      checkOut: this.results.checkOut.split('T')[0]
    });
  }

  get sortedRooms(): Room[] {
    if (!this.results) {
      return [];
    }

    const direction = this.sortDirection === 'asc' ? 1 : -1;

    return [...this.results.rooms].sort((a, b) => {
      if (this.sortField === 'totalStayPrice') {
        return (a.totalStayPrice - b.totalStayPrice) * direction;
      }

      return (this.getRatingValue(a.starRating) - this.getRatingValue(b.starRating)) * direction;
    });
  }

  sortBy(field: 'totalStayPrice' | 'starRating') {
    if (this.sortField === field) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
      return;
    }

    this.sortField = field;
    this.sortDirection = 'asc';
  }

  getSortIndicator(field: 'totalStayPrice' | 'starRating'): string {
    if (this.sortField !== field) {
      return '';
    }

    return this.sortDirection === 'asc' ? '↑' : '↓';
  }

  formatCancellationPolicy(value: Room['cancellationPolicy'] | string | number | boolean | null | undefined): string {
    if (value === 1 || value === '1' || value === true) {
      return 'Yes';
    }

    if (value === 0 || value === '0' || value === false) {
      return 'No';
    }

    if (value == null || value === '') {
      return '—';
    }

    return String(value);
  }

  search() {
    this.error = null;
    const destination = this.destination.trim();

    if (!destination || !this.checkIn || !this.checkOut) {
      this.error = 'Destination, check-in, and check-out dates are required';
      return;
    }

    if (!this.isValidDate(this.checkIn) || !this.isValidDate(this.checkOut)) {
      this.error = 'Please provide valid dates in YYYY-MM-DD format';
      return;
    }

    if (this.checkIn < this.today) {
      this.error = 'Check-in date cannot be in the past';
      return;
    }

    if (this.checkOut <= this.checkIn) {
      this.error = 'Check-out date must be after check-in date';
      return;
    }

    this.loading = true;
    this.hotelService.searchHotels(destination, this.checkIn, this.checkOut, this.roomType)
      .subscribe({
        next: (data) => {
          this.results = data;
          this.destination = destination;
          this.loading = false;
        },
        error: (err) => {
          this.error = err.error?.error || 'Search failed';
          this.loading = false;
        }
      });
  }

  private isValidDate(value: string): boolean {
    return /^\d{4}-\d{2}-\d{2}$/.test(value) && !Number.isNaN(Date.parse(`${value}T00:00:00`));
  }

  private getRatingValue(value: Room['starRating']): number {
    if (!value) {
      return -1;
    }

    const parsed = Number(value);
    return Number.isFinite(parsed) ? parsed : -1;
  }
}
