import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {Disturbance} from '../interfaces/Disturbance';
import { PageEvent } from '@angular/material/paginator';
import { PaginatedResponse } from '../interfaces/PaginatedResponse';


@Injectable({
  providedIn: 'root'
})
export class SearchService {
  private baseUrl = 'http://localhost:5217/RailApi';

  constructor(private http: HttpClient) {}

  getDisturbances(
    pageIndex: number,
    pageSize: number
  ): Observable<PaginatedResponse<Disturbance>> {
    return this.http.get<PaginatedResponse<Disturbance>>(
      `${this.baseUrl}/${pageIndex}/${pageSize}/disturbance`
    );
  }

  // TODO: NEEDS ADJUSTMENT
  getDisturbancesSorted(
    pageIndex: number,
    pageSize: number,
    sortingCriteria: string
  ): Observable<PaginatedResponse<Disturbance>> {
    return this.http.get<PaginatedResponse<Disturbance>>(
      `${this.baseUrl}/${pageIndex}/${pageSize}/${sortingCriteria}/disturbance`
    );
  }

  searchDisturbancesWithTokenization(
    searchTerm: string,
    event: PageEvent
  ): Observable<PaginatedResponse<Disturbance>> {
    return this.http.get<PaginatedResponse<Disturbance>>(
      `${this.baseUrl}/${searchTerm}/${event.pageIndex}/${event.pageSize}/disturbance/tokenization`
    );
  }

  // TODO: NEEDS ADJUSTMENT
  searchDisturbancesWithTokenizationSorted(
    searchTerm: string,
    event: PageEvent,
    sortingCriteria: string
  ): Observable<PaginatedResponse<Disturbance>> {
    return this.http.get<PaginatedResponse<Disturbance>>(
      `${this.baseUrl}/${searchTerm}/${event.pageIndex}/${event.pageSize}/${sortingCriteria}/disturbance/tokenization`
    );
  }
}
