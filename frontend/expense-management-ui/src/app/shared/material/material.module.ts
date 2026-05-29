import { NgModule } from "@angular/core";

import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule }  from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatInputModule } from '@angular/material/input';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatProgressSpinnerModule} from "@angular/material/progress-spinner"


@NgModule({
    exports: [
        MatToolbarModule,
        MatButtonModule,
        MatIconModule,
        MatCardModule,
        MatTableModule,
        MatInputModule,
        MatSidenavModule,
        MatListModule,
        MatFormFieldModule,
        MatProgressSpinnerModule,
        MatCardModule
    ]
})

export class MaterialModule{}