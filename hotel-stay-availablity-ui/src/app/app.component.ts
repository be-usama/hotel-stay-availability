import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SearchComponent } from './components/search.component';
import { ReservationComponent } from './components/reservation.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, SearchComponent, ReservationComponent],
  template: `
    <div class="app-container">
      <header class="app-header">
        <h1>🏨 Hotel Stay Availability</h1>
        <p>Search and book hotels online</p>
      </header>
      
      <main class="app-main">
        <app-search></app-search>
        <app-reservation></app-reservation>
      </main>

      <footer class="app-footer">
        <p>&copy; 2026 SkyRoute Platform. All rights reserved.</p>
      </footer>
    </div>
  `,
  styles: [`
    .app-container {
      min-height: 100vh;
      display: flex;
      flex-direction: column;
      background-color: #f5f5f5;
    }

    .app-header {
      background-color: #1976d2;
      color: white;
      padding: 30px 20px;
      text-align: center;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    }

    .app-header h1 {
      margin: 0;
      font-size: 2.5rem;
    }

    .app-header p {
      margin: 10px 0 0 0;
      font-size: 1.1rem;
      opacity: 0.9;
    }

    .app-main {
      flex: 1;
      padding: 30px 20px;
    }

    .app-footer {
      background-color: #333;
      color: white;
      text-align: center;
      padding: 20px;
      margin-top: 30px;
    }

    .app-footer p {
      margin: 0;
    }
  `]
})
export class AppComponent { }
