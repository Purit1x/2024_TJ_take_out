<script setup>
import { useStore } from "vuex";
import { ElMessage } from "element-plus";
import { ref, onMounted, watch } from 'vue';  
import { useRouter } from 'vue-router';
import { CreateSpecialOffer, EditSpecialOffer, DeleteSpecialOffer, GetSpecialOffer } from '@/api/merchant';

const store = useStore();
const router = useRouter();
const gobackHome = () => {
    router.push('/merchant-home');
}
const merchant = ref({}); // 初始化商家信息对象
const offers=ref([]);  //购物车物品列表

const editingOfferId = ref(null);  // 控制当前编辑的活动ID
const editedOffer = ref({ minPrice: '', amountRemission: '' });

const showInputFields = ref(false);  // 控制输入框显示的状态
const newOffer = ref({
  minPrice: '',
  amountRemission: ''
});

// 数字输入验证
const validatePositiveInteger = (value) => {
    if (!/^\d+$/.test(value)) {
        ElMessage.error('请输入正整数');
        return '';
    }
    return value;
}

onMounted(async() => {  
    //获取商铺用户信息
    const merchantData = store.state.merchant; 
    if (merchantData) {  
        merchant.value = merchantData;  
    } else {  
        router.push('/login');
    }  
    const merchantId = merchant.value.MerchantId;

    const data = await GetSpecialOffer(merchantId); 
    if (data) {  
        offers.value = data.data; // 假设后端返回的offer数据是一个数组
    }  
})

// 创建offer函数
const addSpecialOffer = async(offer) => {
    const merchantId = merchant.value.MerchantId;
    const minPrice = validatePositiveInteger(newOffer.value.minPrice);
    const amountRemission = validatePositiveInteger(newOffer.value.amountRemission);

    if (!minPrice || !amountRemission) {
        // 如果验证失败，返回
        return;
    }

    // 调用API，将购物车项保存到数据库    
    try {
      const offerData = {
        MerchantId: merchantId,  // 商户ID
        MinPrice: offer.minprice,  // 满多少元
        AmountRemission: offer.remission  // 减多少元
      };

        const response = await CreateSpecialOffer(offerData); // 调用 API 
        ElMessage.success(response.msg); // 显示成功消息  

        // 将新创建的活动加入到 offers 列表中
        offers.value.push(response.data);

        // 清空输入框
        newOffer.value.minPrice = '';
        newOffer.value.amountRemission = '';
        showInputFields.value = false;  // 隐藏输入框

    } catch (error) {  
      if (error.response && error.response.data) {  
        const errorCode = error.response.data.errorCode;  

        if (errorCode === 20000) {  
          ElMessage.error('添加失败');  
        } 
        else {  
          ElMessage.error('发生未知错误');  
        }  
      } 
      else {  
        console.info(error.toString());
        ElMessage.error('网络错误，请重试');  
      }  
    }  
};

const startEditingOffer = (offer) => {
    editingOfferId.value = offer.offerId;
    editedOffer.value.minPrice = offer.minPrice;
    editedOffer.value.amountRemission = offer.amountRemission;
};

const cancelEditingOffer = () => {
    editingOfferId.value = null;
};

// 修改offer函数
const updateSpecialOffer = async(offer) => {
    const merchantId = merchant.value.MerchantId;
    const minPrice = validatePositiveInteger(newOffer.value.minPrice);
    const amountRemission = validatePositiveInteger(newOffer.value.amountRemission);

    if (!minPrice || !amountRemission) {
        // 如果验证失败，返回
        return;
    }

    try {
        const offerData = {
            OfferId: offer.offerId,
            MerchantId: merchantId,
            MinPrice: editedOffer.value.minPrice,
            AmountRemission: editedOffer.value.amountRemission
        };

        const response = await EditSpecialOffer(offerData);
        ElMessage.success(response.msg || '满减活动修改成功');

        // 更新 offers 列表中的数据
        const index = offers.value.findIndex(o => o.offerId === offer.offerId);
        if (index !== -1) {
            offers.value[index].minPrice = editedOffer.value.minPrice;
            offers.value[index].amountRemission = editedOffer.value.amountRemission;
        }

        editingOfferId.value = null;

    } catch (error) {  
        if (error.response && error.response.data) {  
            ElMessage.error(error.response.data.msg || '修改失败');  
        } else {  
            ElMessage.error('网络错误，请重试');  
        }  
    }  
};

const deleteSpecialOffer = async(offerId) => {
    try {
        const response = await DeleteSpecialOffer(offerId);
        ElMessage.success(response.msg || '满减活动删除成功');

        // 从 offers 列表中移除
        offers.value = offers.value.filter(o => o.offerId !== offerId);

        // 如果当前活动正在被编辑，取消编辑状态
        if (editingOfferId.value === offerId) {
            editingOfferId.value = null;
        }

    } catch (error) {  
        if (error.response && error.response.data) {  
            ElMessage.error(error.response.data.msg || '删除失败');  
        } else {  
            ElMessage.error('网络错误，请重试');  
        }  
    }  
};

</script>

<template>
    <div class="content">
        <header>满减活动</header>
        <button @click="gobackHome()">返回</button>
        <div>
            <ul>
            <li v-for="offer in offers" :key="offer.offerId">
                <div v-if="editingOfferId !== offer.offerId">
                    满{{offer.minPrice}}元&nbsp;&nbsp;减{{offer.amountRemission}}元
                    <button @click="startEditingOffer(offer)">修改</button>
                    <button @click="deleteSpecialOffer(offer.offerId)">删除</button>
                </div>
                <div v-else>
                    <el-input v-model="editedOffer.minPrice" placeholder="修改满多少元" @input="editedOffer.minPrice = validatePositiveInteger($event)" />
                    <el-input v-model="editedOffer.amountRemission" placeholder="修改减多少元" @input="editedOffer.amountRemission = validatePositiveInteger($event)" />
                    <el-button type="primary" @click="updateSpecialOffer(offer)">保存</el-button>
                    <el-button @click="cancelEditingOffer">取消</el-button>
                </div>
            </li>
            </ul>
        </div>

        <button @click="showInputFields = true">创建满减活动</button>
        <div v-if="showInputFields">
            <el-input v-model="newOffer.minPrice" placeholder="请输入满多少元" @input="editedOffer.minPrice = validatePositiveInteger($event)" />
            <el-input v-model="newOffer.amountRemission" placeholder="请输入减多少元" @input="editedOffer.minPrice = validatePositiveInteger($event)" />
            <el-button type="primary" @click="addSpecialOffer({ minprice: newOffer.minPrice, remission: newOffer.amountRemission })">提交</el-button>
            <el-button @click="showInputFields = false">取消</el-button>
        </div>

    </div>
</template>