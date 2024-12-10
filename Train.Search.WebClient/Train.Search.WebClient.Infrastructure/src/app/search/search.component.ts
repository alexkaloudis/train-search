import { Component, EventEmitter, OnInit, Output, inject } from '@angular/core';
import { SearchService } from './search.service';
import { Disturbance } from '../interfaces/Disturbance';
import { Observable } from 'rxjs';
import { PageEvent } from '@angular/material/paginator';
import { PaginatedResponse } from '../interfaces/PaginatedResponse';

@Component({
  selector: 'app-search',
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.css']
})


export class SearchComponent implements OnInit {
  @Output('ngModelChange') update = new EventEmitter();
  sortingOptions = [
    {value: 'relevance', viewValue: 'Relevance'},
    {value: 'date', viewValue: 'Date'},
    {value: 'title', viewValue: 'Title'}
  ];

  // Model for two-way binding
  selectedSort: string = "";

  // Optional: Track last selected sort for demonstration
  lastSelectedSort: string = "";
  pageIndex: number = 0;
  pageSize: number = 10;
  length: number = 0;

  currentSearchTerm = '';
  isLoading = false;
  errorMessage = '';
  disturbancesList: Disturbance[] = [];

  private searchService = inject(SearchService);

  constructor() {}

  ngOnInit() {
    this.loadDisturbances();
  }

  loadDisturbances(event?: PageEvent) {
    this.isLoading = true;

    const pageEvent = event || { pageIndex: this.pageIndex, pageSize: this.pageSize } as PageEvent;
    var observable: Observable<PaginatedResponse<Disturbance>> =  this.currentSearchTerm
      ? this.searchService.searchDisturbancesWithTokenization(this.currentSearchTerm, pageEvent)
      : this.searchService.getDisturbances(pageEvent.pageIndex, pageEvent.pageSize);

      observable.subscribe({
        next: (response) => {
          this.disturbancesList = response.items;
          this.pageIndex = pageEvent.pageIndex;
          this.pageSize = pageEvent.pageSize;
          this.length = response.totalCount;
          this.isLoading = false;
        },
        error: (err) => {
          console.error('Error loading disturbances', err);
          this.errorMessage = 'Error loading disturbances.';
          this.isLoading = false;
          this.disturbancesList = [];
        }
      });
    }


  loadSortedDisturbances(sortingCriteria: string, event?: PageEvent) {
    this.isLoading = true;

    const pageEvent = event || { pageIndex: this.pageIndex, pageSize: this.pageSize } as PageEvent;
    var observable: Observable<PaginatedResponse<Disturbance>> = new Observable<PaginatedResponse<Disturbance>>;
      observable = this.currentSearchTerm
      ? this.searchService.searchDisturbancesWithTokenizationSorted(this.currentSearchTerm, pageEvent, sortingCriteria)
      : this.searchService.getDisturbancesSorted(pageEvent.pageIndex, pageEvent.pageSize, sortingCriteria);


    observable.subscribe({
      next: (response) => {
        this.disturbancesList = response.items;
        this.pageIndex = pageEvent.pageIndex;
        this.pageSize = pageEvent.pageSize;
        this.length = response.totalCount;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading disturbances', err);
        this.errorMessage = 'Error loading disturbances.';
        this.isLoading = false;
        this.disturbancesList = [];
      }
    });
  }


  filterResults(searchTerm: string) {
    this.currentSearchTerm = searchTerm;
    this.pageIndex = 0;
    this.loadDisturbances();
  }


  onPageChange(event: PageEvent) {
    this.loadDisturbances(event);
  }

  onOptionChange(value: string) {
    console.log(`Selected sort: ${value}`);
    this.pageIndex = 0;
    const pageEvent = event || { pageIndex: this.pageIndex, pageSize: this.pageSize } as PageEvent;
    if (value) {
      this.loadSortedDisturbances(value);
    }else{
      console.log("something went wrong");
    }
  }

}
