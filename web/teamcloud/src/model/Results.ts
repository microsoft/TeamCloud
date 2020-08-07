export interface DataResult<T> {
    data: T
    code: number
    status: string
    location: string
}

export interface StatusResult {
    code: number
    status: string
    state: string
    stateMessage: string
    commandId: string
    location: string
    errors: ResultError[]
}

export interface ErrorResult {
    code: number
    status: string
    errors: ResultError[]
}

export interface ResultError {
    code: string
    message: string
    errors: ValidationError[]
}

export interface ValidationError {
    field: string
    message: string
}
