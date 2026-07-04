import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HotelService } from '../services/hotel.service';
import { SearchResponse } from '../models/hotel';

@Component({
  selector: 'app-search',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.css']
})
export class SearchComponent {
  destination = '';
  checkIn = '';
  checkOut = '';
  roomType = '';
  results: SearchResponse | null = null;
  error: string | null = null;
  loading = false;

  constructor(private hotelService: HotelService) { }

  search() {
    this.error = null;
    if (!this.destination || !this.checkIn || !this.checkOut) {
      this.error = 'Destination, check-in, and check-out dates are required';
      return;
    }

    this.loading = true;
    this.hotelService.searchHotels(this.destination, this.checkIn, this.checkOut, this.roomType)
      .subscribe({
        next: (data) => {
          this.results = data;
          this.loading = false;
        },
        error: (err) => {
          this.error = err.error?.error || 'Search failed';
          this.loading = false;
        }
      });
  }
}
