export interface Campaign {
    campId: string;
    campDesc: string;
    statusInd: 'A' | 'I';
    createdBy: string;
    createdAt: string;
}