# Use Node.js 20 LTS Alpine
FROM node:20-alpine

# Set working directory
WORKDIR /app

# Copy frontend package files
COPY frontend/package*.json ./

# Install dependencies
RUN npm ci --legacy-peer-deps

# Copy frontend source
COPY frontend/ ./

# Build the application
RUN npm run build

# Expose port
EXPOSE ${PORT:-5174}

# Start the preview server with PORT variable
CMD sh -c "npm run preview -- --host 0.0.0.0 --port ${PORT:-5174}"
