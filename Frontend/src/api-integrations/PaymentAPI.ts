import { PaymentInfo } from './Interfaces/API_Interfaces.ts';
import { TokenNeedUpdate } from './TokenObserver.ts';
import axios from 'axios';


var PAYMENT_API_URL = import.meta.env.VITE_PAYMENT_API;

const handlePayOperate = async (infoPay: PaymentInfo) => {
    try {
        const response = await axios.post(`${PAYMENT_API_URL}/api/Payment/Pay`, infoPay);

        if (response.status === 200) {
            return true;
        }

        return false;
    }
    catch (error) {
        return false;
    }

}

export { handlePayOperate }