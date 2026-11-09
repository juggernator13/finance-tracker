import http from 'k6/http';
import { check, sleep } from 'k6';

const GW = __ENV.GW || 'http://localhost:5000';

export const options = {
    stages: [
        { duration: '10s', target: 50 },
        { duration: '30s', target: 50 },
        { duration: '5s', target: 0 },
    ],
    thresholds: {
        http_req_failed: ['rate<0.01'],
        http_req_duration: ['p(95)<500'],
    },
};

export function setup() {
    const res = http.post(`${GW}/api/auth/login`,
        JSON.stringify({ email: __ENV.USER_EMAIL, password: __ENV.USER_PASSWORD }),
        { headers: { 'Content-Type': 'application/json' } });
    check(res, { 'login ok': (r) => r.status === 200 });
    return { token: res.json('accessToken') };
}

export default function (data) {
    const res = http.get(`${GW}/api/reports/by-category?year=2026&month=10`,
        { headers: { Authorization: `Bearer ${data.token}` } });
    check(res, { 'status 200': (r) => r.status === 200 });
    sleep(0.1);
}