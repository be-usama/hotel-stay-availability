import { RoomType, CancellationPolicy, DocumentType } from './enums';

export interface Room {
  roomId: string;
  providerName: string;
  roomType: RoomType;
  perNightRate: number;
  totalStayPrice: number;
  cancellationPolicy: CancellationPolicy;
  starRating?: string;
  amenities?: string[];
  numberOfNights: number;
}

export interface SearchResponse {
  rooms: Room[];
  destination: string;
  checkIn: string;
  checkOut: string;
}

export interface ReservationRequest {
  roomId: string;
  guestName: string;
  documentType: DocumentType;
  documentNumber: string;
  destination: string;
  checkIn: string;
  checkOut: string;
}

export interface ReservationConfirmation {
  referenceNumber: string;
  guestName: string;
  providerName: string;
  roomType: RoomType;
  checkIn: string;
  checkOut: string;
  totalPrice: number;
  cancellationPolicy: CancellationPolicy;
  status: string;
}
