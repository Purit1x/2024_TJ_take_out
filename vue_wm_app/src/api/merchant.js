//导入request.js请求工具
import request from '@/utils/request.js'
import axios from 'axios'  
// 设置基本URL，这里使用你后端的地址  
const BASE_URL = 'http://localhost:5079/api'; 

// 用户注册服务  
export const merchantRegisterService = async (merchantData) => {  
    try {  
        const response = await axios.post(`${BASE_URL}/Merchant/register`, merchantData);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    }  
}
//提供调用登录接口的函数
export const merchantLoginService = async(loginData) => {
    try {  
        const response = await axios.post(`${BASE_URL}/Merchant/login`, loginData); 
        return response.data;
    } catch (error) {  
        throw error;   
    }  
}

export const updateMerchant = async(data) => {
    try {  
        const response = await axios.put(`${BASE_URL}/Merchant/merchantEdit`, data);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    } 
}

export const merchantInfo = async(id) => {
    try {  
        const response = await axios.get(`${BASE_URL}/Merchant/merchantSearch?MerchantId=${id}`);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    }
}
export const walletRecharge=async(id,addMoney) => {
    try {  
        const response = await axios.put(`${BASE_URL}/Merchant/recharge?merchantId=${id}&addMoney=${addMoney}`);  
        return response.data; // 返回后端返回的数据
    } catch (error) {  
        throw error;   
    }
}
export const searchDishes = async(id) => {
    try {  
        const response = await axios.get(`http://localhost:5079/api/Merchant/dishSearch?merchantId=${id}`);
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    }
}
export const getAllStationInfos=async() => {
    try {  
        const response = await axios.get(`${BASE_URL}/Merchant/GetStations`);  
        const stationsInfo = response.data.data; // 获取站点数据   
        // 创建一个数组来保存包含 StationId 和对应的经纬度信息  
        const stationsWithCoordinates = [];  

        // 遍历每个站点，调用高德地图 API 获取经纬度  
        for (const station of stationsInfo) {  
            const address = encodeURIComponent(station.stationAddress); // 对地址进行 URL 编码  
            const geoResponse = await axios.get(`https://restapi.amap.com/v3/geocode/geo?address=${address}&output=JSON&key=bf0d646ec03956c4f1fbf1215faa3864`);  

            // 从高德地图 API 响应中提取经纬度  
            const location = geoResponse.data.geocodes && geoResponse.data.geocodes.length > 0  
                ? geoResponse.data.geocodes[0].location.split(',')  
                : [null, null]; // 如果没有获取到经纬度，返回 null  

            // 将 StationId 和经纬度（经度、纬度）一起推入结果数组  
            stationsWithCoordinates.push({  
                StationId: station.stationId,  
                Longitude: location[0], // 经度  
                Latitude: location[1]    // 纬度  
            });  
        }
        return stationsWithCoordinates; // 返回包含 StationId 和经纬度的数组  
    } catch (error) {  
        throw error;   
    }
}
export const assignStationToMerchant=async(address) => {
    try {  
        // 获取所有站点的经纬度信息  
        const stationsWithCoordinates = await getAllStationInfos();  

        // 获取目标地址的经纬度  
        const encodedAddress = encodeURIComponent(address); // 对地址进行 URL 编码  
        const geoResponse = await axios.get(`https://restapi.amap.com/v3/geocode/geo?address=${encodedAddress}&output=JSON&key=bf0d646ec03956c4f1fbf1215faa3864`); // 替换为您的高德地图 API Key  

        const targetLocation = geoResponse.data.geocodes && geoResponse.data.geocodes.length > 0  
            ? geoResponse.data.geocodes[0].location.split(',')  
            : [null, null]; // 如果没有获取到经纬度，返回 null  

        if (targetLocation[0] === null) return null; // 如果没有找到目标地址的经纬度，返回 null  

        const targetLongitude = parseFloat(targetLocation[0]);  
        const targetLatitude = parseFloat(targetLocation[1]);  

        let nearestStationId = null;  
        let minDistance = Infinity;  

        // 计算每个站点与目标地址之间的距离  
        for (const station of stationsWithCoordinates) {  
            const stationLongitude = parseFloat(station.Longitude);  
            const stationLatitude = parseFloat(station.Latitude);  

            // 使用 Haversine 公式计算距离（单位：千米）  
            const distance = haversineDistance(targetLatitude, targetLongitude, stationLatitude, stationLongitude);  

            // 如果当前站点距离更近，则更新最近站点 ID 和最小距离  
            if (distance < minDistance) {  
                minDistance = distance;  
                nearestStationId = station.StationId;  
            }  
        }  
        return nearestStationId; // 返回最近站点的 ID  
    } catch (error) {  
        throw error; // 抛出错误  
    }  
}
// Haversine 公式计算两点距离（单位：千米）  
const haversineDistance = (lat1, lon1, lat2, lon2) => {  
    const R = 6371; // 地球半径，单位：千米  
    const dLat = degreesToRadians(lat2 - lat1);  
    const dLon = degreesToRadians(lon2 - lon1);  
    const a =  
        Math.sin(dLat / 2) * Math.sin(dLat / 2) +  
        Math.cos(degreesToRadians(lat1)) * Math.cos(degreesToRadians(lat2)) *  
        Math.sin(dLon / 2) * Math.sin(dLon / 2);  
    const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));  
    return R * c; // 返回距离，单位：千米  
};  
// 辅助函数：将度转化为弧度  
const degreesToRadians = (degrees) => {  
    if (typeof degrees !== 'number') {  
        throw new Error('Input must be a number');  
    }  
    return degrees * (Math.PI / 180);  
};  
export const EditMerchantStation=async(data) => {
    try {  
        const response = await axios.put(`${BASE_URL}/Merchant/editMerchantStation`, data);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {  
        throw error;   
    }
}
export const AssignStation=async(data) => {
    try {  
        const response = await axios.post(`${BASE_URL}/Merchant/assignStation`, data);  
        return response.data; // 返回后端返回的数据  
    } catch (error) {   
        throw error;   
    }
}