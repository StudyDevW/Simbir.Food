import { useState, useRef, useEffect } from 'react'

class loadingComponent {
    
    private loadingFlag: boolean = false;

    private timerId: number | null = null;
    private isRunning: boolean = false;
    private isCompleted: boolean = false;

    public startLoading(): void {
        this.loadingFlag = true;
    }

    public endLoading(): void {
        this.loadingFlag = false;
    }

    public getLoading(): boolean {
        return this.loadingFlag;
    }

    public startLoadingAnimation(): void {
        if (this.isRunning) {
            console.log('Таймер уже запущен.');
            return;
        }

        this.isRunning = true;
        this.isCompleted = false;

        console.log('Анимация загрузки: Запущена');

        this.timerId = setTimeout(() => {
            this.isRunning = false;
            this.isCompleted = true;
            console.log('Анимация загрузки: Завершена');
        }, 1000);
    }


    public stopLoadingTimer() {
        if (!this.isRunning) {
            return;
        }

        clearTimeout(this.timerId!);
        this.isRunning = false;
        this.isCompleted = false;
    }


    public getStatusLoadingAnimation() {
        return {
            isRunning: this.isRunning,
            isCompleted: this.isCompleted,
        };
    }


    // public setLoadingReset(): void {
    //     this.loadingStart = true;
    // }
}

export { loadingComponent }