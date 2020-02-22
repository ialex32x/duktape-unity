
export class MyClass {
    private x: number = 0;

    constructor() {
    }

    update() {
        // 尝试重现调试器断点bug
        
        this.x += 1;
    }
}