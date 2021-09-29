const sanitizedStack = () => {
	var stack = (new Error()).stack;
	if (stack) {
		var lines = stack.split('\n');
		lines.splice(1, 3);
		return lines.join('\n');
	}
	return stack;
} 

export class HttpError extends Error {
	
	constructor(public statusCode: number, message?: string) {
		super(message);
		this.statusCode = statusCode;
		this.stack = sanitizedStack();
	}
}