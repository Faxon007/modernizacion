export interface ApiResponse<T> {
    success: boolean;
    data: T;
    errorCode: string;
    errorMessage: string;
}