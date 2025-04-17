import { Component, OnInit, AfterViewInit, ViewChild, ElementRef, AfterViewChecked } from '@angular/core';
import { AdminService } from '../../../Services/AdminServices/admin.service';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import Bugsnag from '@bugsnag/js';

declare const echarts: any;

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './admin-dashboard.component.html',
  styleUrl: './admin-dashboard.component.css'
})
export class AdminDashboardComponent implements OnInit, AfterViewInit, AfterViewChecked {
  @ViewChild('barChartContainer') barChartContainer!: ElementRef;
  @ViewChild('pieChartContainer') pieChartContainer!: ElementRef;

  barChartData: any;
  pieChartData: any;
  barChartInitialized = false;
  pieChartInitialized = false;
  loading = true;
  error = '';

  constructor(
    private adminService: AdminService,
    private router: Router
  ) { }

  ngOnInit(): void {
    // Step 1: Fetch chart data when component initializes
    this.loadChartData();
  }

  ngAfterViewInit(): void {
    // Step 2: Initialize charts once the view is initialized
    this.initChartsWhenReady();
  }

  ngAfterViewChecked(): void {
    // Step 3: Re-check and initialize charts if necessary data is available
    if ((this.barChartData || this.pieChartData) &&
      (!this.barChartInitialized || !this.pieChartInitialized)) {
      this.initChartsWhenReady();
    }
  }

  loadChartData(): void {
    this.loading = true;
    this.error = '';

    // Step 4: Fetch data for Bar Chart
    this.adminService.getBarChartData().subscribe({
      next: data => {
        // Step 4.1: Store the data received and initialize the bar chart
        this.barChartData = data;
        this.buildBarChartOptions();
      },
      error: err => {
        console.error('Failed to load bar chart data:', err);
        this.error = 'Failed to load chart data';

        // Send error to Bugsnag
        Bugsnag.notify(err);

      }
    });

    // Step 5: Fetch data for Pie Chart
    this.adminService.getPieChartData().subscribe({
      next: data => {
        // Step 5.1: Store the data received and initialize the pie chart
        this.pieChartData = data;
        this.buildPieChartOptions();
      },
      error: err => {
        console.error('Failed to load pie chart data:', err);
        this.error = this.error || 'Failed to load chart data';
      },
      complete: () => {
        // Step 5.2: Set loading to false once both charts' data are fetched
        this.loading = false;
      }
    });
  }

  initChartsWhenReady(): void {
    // Step 6: Ensure that charts are initialized only when data is available
    if (typeof window === 'undefined' || typeof echarts === 'undefined') return;

    // Step 6.1: Initialize Bar Chart if the data and container are ready
    if (this.barChartData && this.barChartContainer?.nativeElement && !this.barChartInitialized) {
      this.buildBarChartOptions();
    }

    // Step 6.2: Initialize Pie Chart if the data and container are ready
    if (this.pieChartData && this.pieChartContainer?.nativeElement && !this.pieChartInitialized) {
      this.buildPieChartOptions();
    }
  }

  buildBarChartOptions(): void {
    // Step 7: Safely initialize the Bar Chart with the correct data and configuration
    if (this.barChartInitialized || !this.barChartContainer?.nativeElement) return;

    try {
      const myChart = echarts.init(this.barChartContainer.nativeElement);
      const option = {
        tooltip: {
          trigger: 'axis',
          axisPointer: {
            type: 'shadow'
          }
        },
        xAxis: {
          type: 'category',
          data: ['Patients', 'Doctors']
        },
        yAxis: {
          type: 'value'
        },
        series: [
          {
            data: [
              {
                value: this.barChartData.patients,
                itemStyle: {
                  color: '#5470C6'
                }
              },
              {
                value: this.barChartData.doctors,
                itemStyle: {
                  color: '#91CC75'
                }
              }
            ],
            type: 'bar',
            showBackground: true,
            backgroundStyle: {
              color: 'rgba(180, 180, 180, 0.2)'
            }
          }
        ]
      };
      myChart.setOption(option);
      this.barChartInitialized = true;

      // Step 8: Add resize listener to the chart
      window.addEventListener('resize', () => myChart.resize());
    } catch (e) {
      console.error('Failed to initialize bar chart:', e);
      this.error = 'Failed to render bar chart';
    }
  }

  buildPieChartOptions(): void {
    // Step 9: Safely initialize the Pie Chart with the correct data and configuration
    if (this.pieChartInitialized || !this.pieChartContainer?.nativeElement) return;

    try {
      const myChart = echarts.init(this.pieChartContainer.nativeElement);
      const option = {
        tooltip: {
          trigger: 'item'
        },
        legend: {
          top: '5%',
          left: 'center'
        },
        series: [
          {
            name: 'Gender Distribution',
            type: 'pie',
            radius: ['40%', '70%'],
            avoidLabelOverlap: false,
            itemStyle: {
              borderRadius: 10,
              borderColor: '#fff',
              borderWidth: 2
            },
            label: {
              show: false,
              position: 'center'
            },
            emphasis: {
              label: {
                show: true,
                fontSize: '18',
                fontWeight: 'bold'
              }
            },
            labelLine: {
              show: false
            },
            data: this.pieChartData.map((item: any) => ({
              value: item.value,
              name: item.name,
              itemStyle: {
                color: item.name === 'Male' ? '#5470C6' : '#EE6666'
              }
            }))
          }
        ]
      };
      myChart.setOption(option);
      this.pieChartInitialized = true;

      // Step 10: Add resize listener to the chart
      window.addEventListener('resize', () => myChart.resize());
    } catch (e) {
      console.error('Failed to initialize pie chart:', e);
      this.error = 'Failed to render pie chart';
    }
  }

  // Step 11: Navigate to the "Create Patient" page
  navigateToCreatePatient(): void {
    this.router.navigate(['/admin/createPatient']);
  }
  navigateToCreateDoctor(): void {
    this.router.navigate(['/admin/create-doctor']);
  }
}
