import axios from 'axios'
// 设置基本URL，这里使用你后端的地址  
const BASE_URL = 'http://localhost:5079/api';

//提供调用登录接口的函数
export const adminLoginService = async (loginData) => {
    try {
        const response = await axios.post(`${BASE_URL}/Platform/login`, loginData);
        return response.data;
    } catch (error) {
        throw error;
    }
}
export const merchantDeleteService = async (id) => {
    try {
        const response = await axios.delete(`${BASE_URL}/Platform/merchantDelete?merchantId=${id}`);
        return response.data;
    } catch (error) {
        throw error;
    }
}
export const getStationIds = async () => {  //获取站点id
    try {
        const response = await axios.get(`${BASE_URL}/Platform/GetStationIds`);
        return response.data; // 返回后端返回的数据
    } catch (error) {
        throw error;
    }
}
export const getStationsInfo = async (id) => {  //获取站点信息
    try {
        const response = await axios.get(`${BASE_URL}/Platform/stationSearch?stationId=${id}`);
        return response.data; // 返回后端返回的数据  
    } catch (error) {
        throw error;
    }
}
export const stationDeleteService = async (id) => {
    try {
        const response = await axios.delete(`${BASE_URL}/Platform/stationDelete?stationId=${id}`);
        return response.data;
    } catch (error) {
        throw error;
    }
}
export const updateStation = async (data) => {  //更新用户信息
    try {
        const response = await axios.put(`${BASE_URL}/Platform/stationEdit`, data);
        return response.data; // 返回后端返回的数据  
    } catch (error) {
        throw error;
    }
}
export const addStation = async (data) => {  //新增站点
    try {
        const response = await axios.post(`${BASE_URL}/Platform/stationCreate`, data);
        return response.data; // 返回后端返回的数据  
    } catch (error) {
        throw error;
    }
}
export const separateRiders = async () => {  //根据站点是否分配来区分骑手
    try {
        const response = await axios.get(`${BASE_URL}/Platform/SeparateRiders`);
        return response.data; // 返回后端返回的数据
    } catch (error) {
        throw error;
    }
}
export const assignRiderStation = async (data) => {  //为骑手分配站点
    try {
        const response = await axios.post(`${BASE_URL}/Platform/riderStationAssign`, data);
        return response.data; // 返回后端返回的数据
    } catch (error) {
        throw error;
    }
}
export const deleteRiderStation = async (id) => {
    try {
        const response = await axios.delete(`${BASE_URL}/Platform/riderStationDelete?riderId=${id}`);
        return response.data;
    } catch (error) {
        throw error;
    }
}
export const editRiderStation = async (data) => {  //修改骑手站点
    try {
        const response = await axios.put(`${BASE_URL}/Platform/riderStationEdit`, data);
        return response.data; // 返回后端返回的数据
    } catch (error) {
        throw error;
    }
}
export const getCouponIds = async () => {  //获取优惠券id
    try {
        const response = await axios.get(`${BASE_URL}/Platform/GetCouponIds`);
        return response.data; // 返回后端返回的数据
    } catch (error) {
        throw error;
    }
}
export const getCouponsInfo = async (id) => {  //获取站点信息
    try {
        const response = await axios.get(`${BASE_URL}/Platform/couponSearch?couponId=${id}`);
        return response.data; // 返回后端返回的数据  
    } catch (error) {
        throw error;
    }
}
export const editCouponsInfo = async (couponId, isOnShelves) => {  //修改优惠券信息
    try {
        const response = await axios.put(`${BASE_URL}/Platform/couponEdit?couponId=${couponId}&isOnShelves=${isOnShelves}`);
        return response.data; // 返回后端返回的数据  
    } catch (error) {
        throw error;
    }
}
export const createCoupon = async (data) => {  //新增优惠券
    try {
        const response = await axios.post(`${BASE_URL}/Platform/couponCreate`, data);
        return response.data; // 返回后端返回的数据  
    } catch (error) {
        throw error;
    }
}
export const getEcoInfo = async () => { // 获取环保比例
    try {
        const response = await axios.get(`${BASE_URL}/Platform/getEcoOrder`);
        console.log(response.data)
        return response.data; // 返回后端返回的数据  
    } catch (error) {
        throw error;
    }
}

// 获取区域订单量的函数
export const getQuantityByRegion = async (region) => {
    try {
        const response = await axios.get(`${BASE_URL}/Platform/GetQuantityByRegion`, { params: { region } });
        console.log('Response:', response.data); // 增加日志输出以验证响应数据
        return response.data;
    } catch (error) {
        console.error('Error fetching order count:', error);
        throw error;
    }
}

export const getSalesByRegion = async (region) => {
    try {
        const response = await axios.get(`${BASE_URL}/Platform/GetSalesByRegion`, { params: { region } });
        console.log('Response:', response.data); // 增加日志输出以验证响应数据
        return response.data;
    } catch (error) {
        console.error('Error fetching order count:', error);
        throw error;
    }
}